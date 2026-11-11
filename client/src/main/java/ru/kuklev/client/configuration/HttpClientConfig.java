package ru.kuklev.client.configuration;

import org.springframework.context.annotation.Configuration;
import org.springframework.web.service.registry.ImportHttpServices;

@Configuration
@ImportHttpServices(
        group = "math-apis",
        basePackages = "ru.kuklev.client.client"
)
public class HttpClientConfig {

}
