package com.ruoyi.project.system.watch;

import com.alibaba.fastjson2.JSONObject;
import com.ruoyi.common.utils.StringUtils;
import com.ruoyi.project.system.service.ICapsDeviceReportService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.DisposableBean;
import org.springframework.beans.factory.InitializingBean;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketException;
import java.nio.charset.StandardCharsets;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

@Component
public class WatchTcpServer implements InitializingBean, DisposableBean
{
    private static final Logger LOGGER = LoggerFactory.getLogger(WatchTcpServer.class);

    private static final String CMD_HEARTBEAT = "LK";
    private static final String CMD_BP_HR = "bphrt";
    private static final String CMD_OXYGEN = "oxygen";

    @Value("${watch.tcp.port:5011}")
    private int port;

    private final ICapsDeviceReportService capsDeviceReportService;
    private final ExecutorService clientExecutor = Executors.newCachedThreadPool();

    private volatile boolean running;
    private ServerSocket serverSocket;
    private Thread acceptThread;

    public WatchTcpServer(ICapsDeviceReportService capsDeviceReportService)
    {
        this.capsDeviceReportService = capsDeviceReportService;
    }

    @Override
    public void afterPropertiesSet()
    {
        running = true;
        acceptThread = new Thread(this::runServer, "watch-tcp-server");
        acceptThread.setDaemon(true);
        acceptThread.start();
    }

    @Override
    public void destroy()
    {
        running = false;
        closeServerSocket();
        clientExecutor.shutdownNow();
    }

    private void runServer()
    {
        try (ServerSocket socket = new ServerSocket(port)) {
            serverSocket = socket;
            LOGGER.info("watch tcp server started, port={}", port);
            while (running) {
                Socket client = socket.accept();
                clientExecutor.execute(() -> handleClient(client));
            }
        } catch (SocketException e) {
            if (running) {
                LOGGER.error("watch tcp server socket error, port={}", port, e);
            }
        } catch (Exception e) {
            LOGGER.error("watch tcp server failed, port={}", port, e);
        }
    }

    private void handleClient(Socket client)
    {
        try (Socket socket = client;
             InputStream inputStream = socket.getInputStream();
             OutputStream outputStream = socket.getOutputStream()) {
            socket.setSoTimeout(120000);
            ByteArrayOutputStream buffer = new ByteArrayOutputStream();
            int current;
            while ((current = inputStream.read()) != -1) {
                if (current == '[') {
                    buffer.reset();
                }
                buffer.write(current);
                if (current == ']') {
                    String frame = new String(buffer.toByteArray(), StandardCharsets.US_ASCII);
                    String response = handleFrame(frame);
                    if (StringUtils.isNotEmpty(response)) {
                        outputStream.write(response.getBytes(StandardCharsets.US_ASCII));
                        outputStream.flush();
                    }
                    buffer.reset();
                }
            }
        } catch (Exception e) {
            LOGGER.warn("watch tcp client disconnected with error: {}", e.getMessage());
        }
    }

    private String handleFrame(String frame)
    {
        WatchFrame watchFrame = WatchFrame.parse(frame);
        if (watchFrame == null) {
            LOGGER.warn("invalid watch frame: {}", frame);
            return null;
        }

        LOGGER.info("watch frame received: deviceId={}, command={}, content={}",
                watchFrame.deviceId, watchFrame.command, watchFrame.content);

        if (CMD_BP_HR.equalsIgnoreCase(watchFrame.command)) {
            JSONObject metrics = new JSONObject();
            String[] params = watchFrame.params;
            putIfPresent(metrics, "systolic", params, 1);
            putIfPresent(metrics, "diastolic", params, 2);
            putIfPresent(metrics, "heartRate", params, 3);
            saveWatchMetrics(watchFrame, metrics, frame);
            return buildResponse(watchFrame.vendor, watchFrame.deviceId, CMD_BP_HR);
        }

        if (CMD_OXYGEN.equalsIgnoreCase(watchFrame.command)) {
            JSONObject metrics = new JSONObject();
            putIfPresent(metrics, "spo2", watchFrame.params, 2);
            saveWatchMetrics(watchFrame, metrics, frame);
            return buildResponse(watchFrame.vendor, watchFrame.deviceId, CMD_OXYGEN);
        }

        if (CMD_HEARTBEAT.equalsIgnoreCase(watchFrame.command)) {
            return buildResponse(watchFrame.vendor, watchFrame.deviceId, CMD_HEARTBEAT);
        }

        return buildResponse(watchFrame.vendor, watchFrame.deviceId, "x");
    }

    private void saveWatchMetrics(WatchFrame watchFrame, JSONObject metrics, String rawFrame)
    {
        if (metrics.isEmpty()) {
            return;
        }
        int saved = capsDeviceReportService.saveWatchReport(watchFrame.deviceId, metrics, rawFrame);
        LOGGER.info("watch metrics saved: deviceId={}, metrics={}, saved={}",
                watchFrame.deviceId, metrics.toJSONString(), saved);
    }

    private void putIfPresent(JSONObject metrics, String key, String[] params, int index)
    {
        if (params == null || params.length <= index) {
            return;
        }
        String value = params[index];
        if (StringUtils.isNotEmpty(value) && !"0".equals(value)) {
            metrics.put(key, value);
        }
    }

    private String buildResponse(String vendor, String deviceId, String content)
    {
        String length = String.format("%04X", content.length());
        return "[" + vendor + "*" + deviceId + "*" + length + "*" + content + "]";
    }

    private void closeServerSocket()
    {
        if (serverSocket != null) {
            try {
                serverSocket.close();
            } catch (Exception ignored) {
            }
        }
    }

    private static class WatchFrame
    {
        private final String vendor;
        private final String deviceId;
        private final String content;
        private final String command;
        private final String[] params;

        private WatchFrame(String vendor, String deviceId, String content, String command, String[] params)
        {
            this.vendor = vendor;
            this.deviceId = deviceId;
            this.content = content;
            this.command = command;
            this.params = params;
        }

        private static WatchFrame parse(String frame)
        {
            if (StringUtils.isEmpty(frame) || !frame.startsWith("[") || !frame.endsWith("]")) {
                return null;
            }

            String body = frame.substring(1, frame.length() - 1);
            String[] parts = body.split("\\*", 4);
            if (parts.length != 4 || StringUtils.isEmpty(parts[0]) || StringUtils.isEmpty(parts[1])
                    || StringUtils.isEmpty(parts[3])) {
                return null;
            }

            String[] params = parts[3].split(",");
            String command = params.length == 0 ? "" : params[0];
            return new WatchFrame(parts[0], parts[1], parts[3], command, params);
        }
    }
}
