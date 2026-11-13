FROM gcr.io/distroless/java25-debian13:nonroot

WORKDIR /application

ARG JAR_FILE=client/build/libs/client-0.0.1.jar

COPY ${JAR_FILE} app.jar

EXPOSE 8080

ENTRYPOINT ["java", "-XX:+UseZGC", "-XX:+ExitOnOutOfMemoryError", "-jar", "app.jar"]