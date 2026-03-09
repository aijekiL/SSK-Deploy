package com.ruoyi.framework.security.filter;

import javax.servlet.*;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

public class SecretAuthFilter implements Filter {
    private final String secret;
    private final String secretHeader;

    public SecretAuthFilter(String secret, String secretHeader) {
        this.secret = secret;
        this.secretHeader = secretHeader;
    }

    @Override
    public void doFilter(ServletRequest request, ServletResponse response, FilterChain chain)
            throws IOException, ServletException {
        HttpServletRequest httpRequest = (HttpServletRequest) request;
        HttpServletResponse httpResponse = (HttpServletResponse) response;

        String requestSecret = httpRequest.getHeader(secretHeader);

        // 检查请求头中是否包含正确的secret
        if (secret.equals(requestSecret)) {
            // 验证通过，继续请求处理
            chain.doFilter(request, response);
        } else {
            // 验证失败，返回401
            httpResponse.setStatus(HttpServletResponse.SC_UNAUTHORIZED);
            httpResponse.getWriter().write("Invalid secret");
        }
    }
}
