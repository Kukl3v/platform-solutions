package ru.kuklev.app.algorithm;

public class SequentialMatrixSorter {
    public void sort(int[][] matrix) {
        if (matrix == null || matrix.length == 0 || matrix[0].length < 2) {
            return;
        }

        int columns = matrix[0].length;
        int[] minima = matrix[0].clone();
        for (int row = 1; row < matrix.length; row++) {
            for (int column = 0; column < columns; column++) {
                minima[column] = Math.min(minima[column], matrix[row][column]);
            }
        }

        boolean swapped;
        do {
            swapped = false;
            // Нечётная фаза: (1, 2), (3, 4), ...
            for (int column = 1; column < columns - 1; column += 2) {
                if (minima[column] > minima[column + 1]) {
                    swapColumns(matrix, minima, column);
                    swapped = true;
                }
            }

            // Чётная фаза: (0, 1), (2, 3), ...
            for (int column = 0; column < columns - 1; column += 2) {
                if (minima[column] > minima[column + 1]) {
                    swapColumns(matrix, minima, column);
                    swapped = true;
                }
            }
        } while (swapped);
    }

    private static void swapColumns(int[][] matrix, int[] minima, int left) {
        int right = left + 1;
        int temp = minima[left];
        minima[left] = minima[right];
        minima[right] = temp;

        for (int[] row : matrix) {
            temp = row[left];
            row[left] = row[right];
            row[right] = temp;
        }
    }
}
