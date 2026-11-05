package ru.kuklev.server.dto;

import jakarta.validation.constraints.*;

public record MatrixCalculateRequest(@NotEmpty(message = "Матрица не должна быть пустой")
                                  @Size(min = 1, max = 100, message = "Количество строк и колонок матрицы может быть от {min} до {max}")
                                  int[][] matrix) {
}
