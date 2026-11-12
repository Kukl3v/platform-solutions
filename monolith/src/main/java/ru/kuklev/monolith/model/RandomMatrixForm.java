package ru.kuklev.monolith.model;

import jakarta.validation.constraints.AssertTrue;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import ru.kuklev.monolith.validation.MatrixCalculation;

public class RandomMatrixForm {
    @NotNull(message = "Укажите количество строк")
    @Min(value = 1, message = "Количество строк не может быть меньше {value}")
    @Max(value = 100, message = "Количество строк не может быть больше {value}")
    private Integer rows = 3;

    @NotNull(message = "Укажите количество колонок")
    @Min(value = 1, message = "Количество колонок не может быть меньше {value}")
    @Max(value = 100, message = "Количество колонок не может быть больше {value}")
    private Integer cols = 3;

    @NotNull(message = "Укажите минимальное число")
    @Min(value = -10_000, message = "Минимальное число не может быть меньше {value}")
    @Max(value = 10_000, message = "Минимальное число не может быть больше {value}")
    private Integer minNum = 0;

    @NotNull(message = "Укажите максимальное число")
    @Min(value = -10_000, message = "Максимальное число не может быть меньше {value}")
    @Max(value = 10_000, message = "Максимальное число не может быть больше {value}")
    private Integer maxNum = 100;

    @NotNull(groups = MatrixCalculation.class, message = "Сначала сгенерируйте матрицу")
    @Size(groups = MatrixCalculation.class, min = 1, max = 100,
            message = "Количество строк матрицы должно быть от {min} до {max}")
    private int[][] matrix;

    public Integer getRows() {
        return rows;
    }

    public void setRows(Integer rows) {
        this.rows = rows;
    }

    public Integer getCols() {
        return cols;
    }

    public void setCols(Integer cols) {
        this.cols = cols;
    }

    public Integer getMinNum() {
        return minNum;
    }

    public void setMinNum(Integer minNum) {
        this.minNum = minNum;
    }

    public Integer getMaxNum() {
        return maxNum;
    }

    public void setMaxNum(Integer maxNum) {
        this.maxNum = maxNum;
    }

    public int[][] getMatrix() {
        return matrix;
    }

    public void setMatrix(int[][] matrix) {
        this.matrix = matrix;
    }

    @AssertTrue(message = "Минимальное число должно быть меньше или равно максимальному")
    public boolean isValidRange() {
        return minNum == null || maxNum == null || minNum <= maxNum;
    }
}
