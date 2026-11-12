package ru.kuklev.monolith.unit;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import ru.kuklev.monolith.service.MatrixService;
import ru.kuklev.monolith.util.MatrixGenerator;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
class MatrixUnitTest {
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

        assertThat(matrixService.calculateMatrixAverage(matrix)).isEqualTo(5.0);
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

        assertThat(matrixService.calculateMatrixAverage(matrix)).isEqualTo(6.0);
    }

    @Test
    void calculateMatrixAverage_ShouldHandleNegativeNumbersAndZeros() {
        int[][] matrix = {
                {-10, 0},
                {10, -5}
        };

        assertThat(matrixService.calculateMatrixAverage(matrix)).isEqualTo(-1.25);
    }

    @Test
    void calculateMatrixAverage_ShouldReturnSingleElement() {
        assertThat(matrixService.calculateMatrixAverage(new int[][]{{42}})).isEqualTo(42.0);
    }

    @Test
    void calculateMatrixAverage_ShouldHandleIntegerOverflow() {
        int[][] matrix = {
                {Integer.MAX_VALUE, Integer.MAX_VALUE},
                {Integer.MAX_VALUE, Integer.MAX_VALUE}
        };

        assertThat(matrixService.calculateMatrixAverage(matrix)).isEqualTo(Integer.MAX_VALUE);
    }

    @Test
    void generateMatrix_ShouldReturnGeneratedMatrix() {
        int[][] expectedMatrix = {
                {1, 2},
                {3, 4}
        };
        when(matrixGenerator.generate(2, 2, 1, 10)).thenReturn(expectedMatrix);

        assertThat(matrixService.generateMatrix(2, 2, 1, 10)).isDeepEqualTo(expectedMatrix);
    }
}
