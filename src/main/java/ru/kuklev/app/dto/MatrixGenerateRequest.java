package ru.kuklev.app.dto;

import jakarta.validation.constraints.AssertTrue;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;

public record MatrixGenerateRequest(@Min(value = 1, message = "Количество строк матрицы не может быть меньше {value}")
                                    @Max(value = 20000, message = "Количество строк матрицы не может быть больше {value}")
                                    int rows,
                                    @Min(value = 1, message = "Количество колонок матрицы не может быть меньше {value}")
                                    @Max(value = 20000, message = "Количество колонок матрицы не может быть больше {value}")
                                    int cols,
                                    @Min(value = -10_000, message = "Минимальное значение случайного числа не может быть меньше {value}")
                                    @Max(value = 10_000, message = "Минимальное значение случайного числа не может быть больше {value}")
                                    int minNum,
                                    @Min(value = -10_000, message = "Максимальное значение случайного числа не может быть меньше {value}")
                                    @Max(value = 10_000, message = "Максимальное значение случайного числа не может быть больше {value}")
                                    int maxNum,
                                    boolean isFractional) {

    @AssertTrue(message = "Минимальное число должно быть меньше или равно максимальному")
    public boolean isValidRange() {
        return minNum <= maxNum;
    }
}
