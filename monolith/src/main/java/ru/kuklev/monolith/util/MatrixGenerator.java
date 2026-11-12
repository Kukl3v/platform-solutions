package ru.kuklev.monolith.util;

import org.springframework.stereotype.Component;

import java.util.random.RandomGenerator;

@Component
public class MatrixGenerator {
    private final RandomGenerator random = RandomGenerator.getDefault();

    public int[][] generate(int rows, int cols, int minNum, int maxNum) {
        int[][] matrix = new int[rows][cols];

        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < cols; col++) {
                matrix[row][col] = random.nextInt(minNum, maxNum + 1);
            }
        }

        return matrix;
    }
}
