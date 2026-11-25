package ru.kuklev.app.configuration;

import org.springframework.boot.ApplicationRunner;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import ru.kuklev.app.command.MenuCommands;

@Configuration
public class ShellConfig {

    @Bean
    public ApplicationRunner springShellApplicationRunner(
            MenuCommands menuCommands
    ) {
        return _ -> menuCommands.menu();
    }
}
