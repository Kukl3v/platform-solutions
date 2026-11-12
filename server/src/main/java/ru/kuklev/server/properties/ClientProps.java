package ru.kuklev.server.properties;

import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "client")
public record ClientProps(
        String host,
        int port
) {
}
