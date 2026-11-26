package ru.kuklev.app.algorithm;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

public class ThreadPoolMatrixSorter {
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
        try (ThreadPoolExecutor pool = new ThreadPoolExecutor(threads, threads, 0,
                TimeUnit.MILLISECONDS, new LinkedBlockingQueue<>())) {
            boolean oddSwapped;
            boolean evenSwapped;
            do {
                oddSwapped = runPhase(pool, matrix, minima, 1, pairsPerTask);
                evenSwapped = runPhase(pool, matrix, minima, 0, pairsPerTask);
            } while (oddSwapped || evenSwapped);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new IllegalStateException("Сортировка прервана", e);
        } catch (ExecutionException e) {
            throw new IllegalStateException("Ошибка при сортировке", e.getCause());
        }
    }

    private static boolean runPhase(ThreadPoolExecutor pool, int[][] matrix, int[] minima,
                                    int firstColumn, int pairsPerTask)
            throws InterruptedException, ExecutionException {
        int pairCount = (minima.length - firstColumn) / 2;
        List<Callable<Boolean>> tasks = new ArrayList<>();
        for (int pair = 0; pair < pairCount; pair += pairsPerTask) {
            int start = pair;
            int end = Math.min(start + pairsPerTask, pairCount);
            tasks.add(() -> {
                boolean swapped = false;
                for (int index = start; index < end; index++) {
                    int column = firstColumn + 2 * index;
                    if (minima[column] > minima[column + 1]) {
                        swapColumns(matrix, minima, column);
                        swapped = true;
                    }
                }
                return swapped;
            });
        }

        boolean swapped = false;
        for (Future<Boolean> result : pool.invokeAll(tasks)) {
            swapped |= result.get();
        }
        return swapped;
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
