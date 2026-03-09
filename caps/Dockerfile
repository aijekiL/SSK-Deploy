# 使用OpenJDK 8作为基础镜像
FROM openjdk:8-jre

# 设置工作目录
WORKDIR /app

# 复制caps应用的jar包到容器中
COPY target/ruoyi.jar app.jar

# 暴露端口
EXPOSE 5010 19443 1935

# 设置时区
ENV TZ=Asia/Shanghai

# 启动应用
ENTRYPOINT ["java", "-jar", "app.jar"]