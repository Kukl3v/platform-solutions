package ru.kuklev.app.storage;

import org.springframework.stereotype.Component;

import java.util.Arrays;

@Component
public class MatrixStorage {

    private int[][] matrix;

    public void save(int[][] matrix) {
        this.matrix = matrix;
    }

    public int[][] get() {
        if (matrix == null) {
            throw new IllegalStateException("Матрица не сгенерирована");
        }

        return matrix;
    }

    public int[][] copy() {
        return Arrays.stream(get())
                .map(int[]::clone)
                .toArray(int[][]::new);
    }

    public boolean exists() {
        return matrix != null;
    }

    public void clear() {
        matrix = null;
    }
}
