package ru.kuklev.server.unit;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import ru.kuklev.server.dto.MatrixAverageResponse;
import ru.kuklev.server.dto.MatrixCalculateRequest;
import ru.kuklev.server.dto.MatrixGenerateRequest;
import ru.kuklev.server.dto.MatrixResponse;
import ru.kuklev.server.service.MatrixService;
import ru.kuklev.server.util.MatrixGenerator;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
public class MatrixUnitTest {
    @Mock
    private MatrixGenerator matrixGenerator;

    private MatrixService matrixService;

    @BeforeEach
    void setUp() {
        matrixService = new MatrixService(matrixGenerator);
    }

    @Test
    void calculateMatrixAverage_ShouldReturnIntegerAverage() {
        int[][] matrix = {
                {2, 4},
                {6, 8}
        };
        MatrixCalculateRequest request = new MatrixCalculateRequest(matrix);
        MatrixAverageResponse response = matrixService.calculateMatrixAverage(request);

        assertThat(response.average()).isEqualTo(5.0);
    }

    @Test
    void calculateMatrixAverage_ShouldReturnFractionalAverage() {
        int[][] matrix = {
                {1, 2, 6, 7, 10},
                {10, 8, 9, 10, 6},
                {7, 4, 7, 9, 3},
                {9, 8, 1, 6, 5},
                {6, 5, 0, 4, 7}
        };

        MatrixCalculateRequest request = new MatrixCalculateRequest(matrix);
        MatrixAverageResponse response = matrixService.calculateMatrixAverage(request);

        assertThat(response.average()).isEqualTo(6.0);
    }

    @Test
    void calculateMatrixAverage_ShouldHandleNegativeNumbersAndZeros() {
        int[][] matrix = {
                {-10, 0},
                {10, -5}
        };

        MatrixCalculateRequest request = new MatrixCalculateRequest(matrix);
        MatrixAverageResponse response = matrixService.calculateMatrixAverage(request);

        assertThat(response.average()).isEqualTo(-1.25);
    }

    @Test
    void calculateMatrixAverage_ShouldReturnSingleElement() {
        int[][] matrix = {{42}};

        MatrixCalculateRequest request = new MatrixCalculateRequest(matrix);
        MatrixAverageResponse response = matrixService.calculateMatrixAverage(request);

        assertThat(response.average()).isEqualTo(42.0);
    }

    @Test
    void calculateMatrixAverage_ShouldHandleIntegerOverflow() {
        int[][] matrix = {
                {Integer.MAX_VALUE, Integer.MAX_VALUE},
                {Integer.MAX_VALUE, Integer.MAX_VALUE}
        };

        MatrixCalculateRequest request = new MatrixCalculateRequest(matrix);
        MatrixAverageResponse response = matrixService.calculateMatrixAverage(request);

        assertThat(response.average()).isEqualTo(Integer.MAX_VALUE);
    }

    @Test
    void generateMatrix_ShouldReturnGeneratedMatrix() {
        MatrixGenerateRequest request = new MatrixGenerateRequest(2, 2, 1, 10);

        int[][] expectedMatrix = {
                {1, 2},
                {3, 4}
        };

        when(matrixGenerator.generate(2, 2, 1, 10))
                .thenReturn(expectedMatrix);
        MatrixResponse response = matrixService.generateMatrix(request);

        assertThat(response.matrix()).isDeepEqualTo(expectedMatrix);
    }
}
