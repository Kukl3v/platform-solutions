package ru.kuklev.app.service;

import org.springframework.stereotype.Service;
import ru.kuklev.app.dto.MatrixGenerateRequest;
import ru.kuklev.app.dto.MatrixSortResponse;
import ru.kuklev.app.storage.MatrixStorage;
import ru.kuklev.app.util.MatrixGenerator;

import java.util.Arrays;
import java.util.stream.Collectors;

@Service
public class MatrixService {
    private final MatrixGenerator generator;
    private final MatrixStorage storage;

    public static final int MATRIX_MAX_PART = 10;

    public MatrixService(MatrixGenerator generator,
                         MatrixStorage storage) {
        this.generator = generator;
        this.storage = storage;
    }

    public void generateMatrix(MatrixGenerateRequest request) {
        storage.save(generator.generate(request.rows(), request.cols(), request.minNum(), request.maxNum()));
    }

    public MatrixSortResponse runSeqMatrixSort() {

        return new MatrixSortResponse(null, null); //временно
    }

    public MatrixSortResponse runThreadPoolMatrixSort() {

        return new MatrixSortResponse(null, null); //временно
    }

    public MatrixSortResponse runForkJoinMatrixSort() {

        return new MatrixSortResponse(null, null); //временно
    }

    public void clearMatrix() {
        storage.clear();
    }

    public String getPartialMatrix() {
        return Arrays.stream(storage.get())
                .limit(MATRIX_MAX_PART)
                .map(row -> Arrays.toString(Arrays.stream(row).limit(MATRIX_MAX_PART).toArray()))
                .collect(Collectors.joining("\n"));
    }

    public String getPartialMatrix(int[][] matrix) {
        return Arrays.stream(matrix)
                .limit(MATRIX_MAX_PART)
                .map(row -> Arrays.toString(Arrays.stream(row).limit(MATRIX_MAX_PART).toArray()))
                .collect(Collectors.joining("\n"));
    }

    public boolean isMatrixExist() {
        return storage.exists();
    }
}
