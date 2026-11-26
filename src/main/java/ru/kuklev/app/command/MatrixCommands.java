package ru.kuklev.app.command;

import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validator;
import org.springframework.shell.core.command.annotation.Command;
import org.springframework.shell.jline.tui.component.flow.ComponentFlow.ComponentFlowResult;
import org.springframework.shell.jline.tui.component.flow.ComponentFlow.Builder;
import org.springframework.stereotype.Component;
import ru.kuklev.app.dto.MatrixGenerateRequest;
import ru.kuklev.app.service.MatrixService;
import ru.kuklev.app.util.Console;

import java.util.Set;

@Component
public class MatrixCommands {

    private final Builder flowBuilder;
    private final Console console;
    private final Validator validator;
    private  final MatrixService matrixService;

    public MatrixCommands(Builder flowBuilder,
                          Console console,
                          Validator validator,
                          MatrixService matrixService) {
        this.flowBuilder = flowBuilder;
        this.console = console;
        this.validator = validator;
        this.matrixService = matrixService;
    }

    @Command(name = "menu", description = "Меню")
    public void menu() {
        while (true) {
            console.clear();

            console.print("Статус матрицы:");

            if (matrixService.isMatrixExist()) {
                console.success("Матрица сгенерирована\n");
            } else {
                console.warning("Матрица не сгенерирована\n");
            }

            ComponentFlowResult result = flowBuilder.clone()
                    .reset()
                    .withSingleItemSelector("action")
                    .name("Выберите действие с матрицей")
                    .selectItem("Сгенерировать", "generate")
                    .selectItem("Вывести", "print")
                    .selectItem("Очистить", "clear")
                    .selectItem("Выйти", "exit")
                    .and()
                    .build()
                    .run();

            String action = result.getContext()
                    .get("action", String.class);

            console.clear();

            switch (action) {
                case "generate" -> generate();
                case "print" -> print();
                case "clear" -> clear();
                case "exit" -> { return; }
            }
        }
    }

    @Command(name = "generate", description = "Сгенерировать матрицу")
    public void generate() {
        ComponentFlowResult result = flowBuilder.clone()
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
                .withNumberInput("minNum")
                    .name("Минимальное число")
                    .defaultValue(0)
                    .numberClass(Integer.class)
                    .and()
                .withNumberInput("maxNum")
                    .name("Максимальное число")
                    .defaultValue(100)
                    .numberClass(Integer.class)
                    .and()
                .withConfirmationInput("isFractional")
                    .name("Использовать дробные числа?")
                    .defaultValue(false)
                    .and()
                .build()
                .run();

        var context = result.getContext();

        MatrixGenerateRequest request = new MatrixGenerateRequest(
                context.get("rows", Integer.class),
                context.get("cols", Integer.class),
                context.get("minNum", Integer.class),
                context.get("maxNum", Integer.class),
                context.get("isFractional", Boolean.class)
        );

        Set<ConstraintViolation<MatrixGenerateRequest>> violations = validator.validate(request);

        if (!violations.isEmpty()) {
            violations.forEach(v -> console.error(v.getMessage()));
            console.back();
            return;
        }

        console.print("Генерация матрицы...");

        matrixService.generateMatrix(request);

        console.success("Матрица успешно сгенерирована");
        console.back();
    }

    @Command(name = "clear", description = "Очистить матрицу")
    public void clear() {
        matrixService.clearMatrix();

        console.success("Матрица успешно очищена");
        console.back();
    }

    @Command(name = "clear", description = "Вывести матрицу частично")
    public void print() {
        try {
            String matrix = matrixService.getPartialMatrix();
            console.print(String.format("Матрица (часть 10x10):\n%s", matrix));
        } catch (Exception e) {
            console.error(e.getMessage());
        }

        console.back();
    }
}
