package ru.kuklev.client.model;

import jakarta.validation.constraints.AssertTrue;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import ru.kuklev.client.validation.MatrixCalculation;

public class ManualMatrixForm {
    @NotNull(message = "Укажите количество строк")
    @Min(value = 1, message = "Количество строк не может быть меньше {value}")
    @Max(value = 100, message = "Количество строк не может быть больше {value}")
    private Integer rows = 3;

    @NotNull(message = "Укажите количество колонок")
    @Min(value = 1, message = "Количество колонок не может быть меньше {value}")
    @Max(value = 100, message = "Количество колонок не может быть больше {value}")
    private Integer cols = 3;

    @NotNull(groups = MatrixCalculation.class, message = "Сначала задайте размер матрицы")
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

    public int[][] getMatrix() {
        return matrix;
    }

    public void setMatrix(int[][] matrix) {
        this.matrix = matrix;
    }

    @AssertTrue(groups = MatrixCalculation.class,
            message = "Измените размер матрицы перед расчетом")
    public boolean isMatrixShapeValid() {
        if (matrix == null || rows == null || cols == null) {
            return true;
        }
        if (matrix.length != rows) {
            return false;
        }
        for (int[] row : matrix) {
            if (row == null || row.length != cols) {
                return false;
            }
        }
        return true;
    }
}
