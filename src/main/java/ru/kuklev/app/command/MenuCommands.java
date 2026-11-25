package ru.kuklev.app.command;

import org.springframework.shell.core.command.annotation.Command;
import org.springframework.shell.jline.tui.component.flow.ComponentFlow.Builder;
import org.springframework.stereotype.Component;

@Component
public class MenuCommands {

    private final Builder flowBuilder;

    public MenuCommands(Builder flowBuilder) {
        this.flowBuilder = flowBuilder;
    }

    @Command(name = "menu", description = "Меню")
    public String menu() {
        var result = flowBuilder.clone()
                .reset()
                .withSingleItemSelector("action")
                    .name("Выберите действие с матрицей")
                    .selectItem("Сгенерировать", "hello")
                    .selectItem("Упорядочить", "sello")
                    .selectItem("Выйти", "exit")
                    .and()
                .build()
                .run();

        return result.getContext().get("action", String.class);
    }

    @Command(name = "generate", description = "Сгенерировать матрицу")
    public String generate() {
        var result = flowBuilder.clone()
                .reset()
                .withNumberInput("rows")
                    .name("Количество строк")
                    .defaultValue(1000)
                    .numberClass(Integer.class)
                    .and()
                .withNumberInput("cols")
                    .name("Количество столбцов")
                    .defaultValue(1000)
                    .numberClass(Integer.class)
                    .and()
                .withNumberInput("cols")
                    .name("Минимальное число")
                    .defaultValue(0)
                    .numberClass(Integer.class)
                    .and()
                .withNumberInput("cols")
                    .name("Максимальное число")
                    .defaultValue(100)
                    .numberClass(Integer.class)
                    .and()
                .withConfirmationInput("fillRandom")
                    .name("Использовать дробные числа?")
                    .defaultValue(true)
                    .and()
                .build()
                .run();

        return result.getContext().get("action", String.class);
    }

    @Command(name = "sort", description = "Отсортировать матрицу")
    public String sort() {
        return "Sello, world!";
    }
}
