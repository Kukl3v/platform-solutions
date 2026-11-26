package ru.kuklev.app.algorithm;

import java.util.concurrent.ForkJoinPool;
import java.util.concurrent.RecursiveTask;

public class ForkJoinMatrixSorter {
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

        int threads = Math.min(Runtime.getRuntime().availableProcessors(), columns / 2);
        int pairsPerTask = Math.ceilDiv(columns / 2, threads);
        try (ForkJoinPool pool = new ForkJoinPool(threads)) {
            boolean oddSwapped;
            boolean evenSwapped;
            do {
                oddSwapped = pool.invoke(new PhaseTask(matrix, minima, 1, 0, (columns - 1) / 2, pairsPerTask));
                evenSwapped = pool.invoke(new PhaseTask(matrix, minima, 0, 0, columns / 2, pairsPerTask));
            } while (oddSwapped || evenSwapped);
        }
    }

    private static class PhaseTask extends RecursiveTask<Boolean> {
        private final int[][] matrix;
        private final int[] minima;
        private final int firstColumn;
        private final int start;
        private final int end;
        private final int pairsPerTask;

        PhaseTask(int[][] matrix, int[] minima, int firstColumn, int start, int end, int pairsPerTask) {
            this.matrix = matrix;
            this.minima = minima;
            this.firstColumn = firstColumn;
            this.start = start;
            this.end = end;
            this.pairsPerTask = pairsPerTask;
        }

        @Override
        protected Boolean compute() {
            if (end - start <= pairsPerTask) {
                boolean swapped = false;
                for (int pair = start; pair < end; pair++) {
                    int column = firstColumn + 2 * pair;
                    if (minima[column] > minima[column + 1]) {
                        swapColumns(matrix, minima, column);
                        swapped = true;
                    }
                }
                return swapped;
            }

            int middle = start + (end - start) / 2;
            PhaseTask left = new PhaseTask(matrix, minima, firstColumn, start, middle, pairsPerTask);
            PhaseTask right = new PhaseTask(matrix, minima, firstColumn, middle, end, pairsPerTask);
            left.fork();
            boolean rightSwapped = right.compute();
            return left.join() || rightSwapped;
        }
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
