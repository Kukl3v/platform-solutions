FROM gcr.io/distroless/java21-debian12:nonroot

WORKDIR /application

ARG JAR_FILE=server/build/libs/server-0.0.1.jar

COPY ${JAR_FILE} app.jar

EXPOSE 8080

ENTRYPOINT ["java", "-XX:+ExitOnOutOfMemoryError", "-jar", "app.jar"]
