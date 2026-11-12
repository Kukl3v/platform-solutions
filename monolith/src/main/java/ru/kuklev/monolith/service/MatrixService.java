package ru.kuklev.monolith.service;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import ru.kuklev.monolith.util.MatrixGenerator;

import java.util.Arrays;
import java.util.stream.Collectors;

@Service
public class MatrixService {
    private static final Logger LOGGER = LoggerFactory.getLogger(MatrixService.class);

    private final MatrixGenerator matrixGenerator;

    public MatrixService(MatrixGenerator matrixGenerator) {
        this.matrixGenerator = matrixGenerator;
    }

    public int[][] generateMatrix(int rows, int cols, int minNum, int maxNum) {
        int[][] matrix = matrixGenerator.generate(rows, cols, minNum, maxNum);

        LOGGER.info("Matrix generated: {}x{}, range=[{}, {}]", rows, cols, minNum, maxNum);
        return matrix;
    }

    public double calculateMatrixAverage(int[][] matrix) {
        long sum = 0;
        int count = 0;

        for (int[] row : matrix) {
            for (int element : row) {
                sum += element;
                count++;
            }
        }
        double average = (double) sum / count;

        LOGGER.info("Calculated matrix average: average={}, sum={}, count={} \nMatrix:\n{}",
                average,
                sum,
                count,
                formatMatrix(matrix)
        );
        return average;
    }

    private String formatMatrix(int[][] matrix) {
        return Arrays.stream(matrix)
                .map(Arrays::toString)
                .collect(Collectors.joining("\n"));
    }
}
