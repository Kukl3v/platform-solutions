package ru.kuklev.server.util;

import org.springframework.stereotype.Component;

import java.util.random.RandomGenerator;

@Component
public class MatrixGenerator {
    private final RandomGenerator  RANDOM = RandomGenerator.getDefault();

    public int[][] generate(int rows,
                            int cols,
                            int minNum,
                            int maxNum) {
        int[][] matrix = new int[rows][cols];

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                matrix[i][j] = RANDOM.nextInt(minNum, maxNum + 1);
            }
        }

        return matrix;
    }
}
