package ru.kuklev.server.service;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import ru.kuklev.server.dto.MatrixAverageResponse;
import ru.kuklev.server.dto.MatrixCalculateRequest;
import ru.kuklev.server.dto.MatrixGenerateRequest;
import ru.kuklev.server.dto.MatrixResponse;
import ru.kuklev.server.util.MatrixGenerator;

import java.util.Arrays;
import java.util.stream.Collectors;

@Service
public class MatrixService {
    private final MatrixGenerator matrixGenerator;

    private static final Logger LOGGER = LoggerFactory.getLogger(MatrixService.class);

    public MatrixService(MatrixGenerator matrixGenerator) {
        this.matrixGenerator = matrixGenerator;
    }

    public MatrixResponse generateMatrix(MatrixGenerateRequest request) {
        int[][] matrix = matrixGenerator.generate(
                request.rows(),
                request.cols(),
                request.minNum(),
                request.maxNum()
        );

        LOGGER.info("Matrix generated: {}x{}, range=[{}, {}]",
                request.rows(),
                request.cols(),
                request.minNum(),
                request.maxNum()
        );

        return new MatrixResponse(matrix);
    }

    public MatrixAverageResponse calculateMatrixAverage(MatrixCalculateRequest request) {
        long sum = 0;
        int count = 0;

        for (int[] row : request.matrix()) {
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
                formatMatrix(request.matrix())
        );

        return new MatrixAverageResponse(average);
    }

    private String formatMatrix(int[][] matrix) {
        return Arrays.stream(matrix)
                .map(Arrays::toString)
                .collect(Collectors.joining("\n"));
    }
}
