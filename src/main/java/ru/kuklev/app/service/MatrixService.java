package ru.kuklev.app.service;

import org.springframework.stereotype.Service;
import ru.kuklev.app.dto.MatrixGenerateRequest;
import ru.kuklev.app.storage.MatrixStorage;
import ru.kuklev.app.util.MatrixGenerator;

import java.util.Arrays;
import java.util.stream.Collectors;

@Service
public class MatrixService {
    private final MatrixGenerator generator;
    private final MatrixStorage storage;

    public MatrixService(MatrixGenerator generator,
                         MatrixStorage storage) {
        this.generator = generator;
        this.storage = storage;
    }

    public void generateMatrix(MatrixGenerateRequest request) {
        storage.save(generator.generate(request.rows(), request.cols(), request.minNum(), request.maxNum()));
    }

    public void clearMatrix() {
        storage.clear();
    }

    public String getPartialMatrix() {
        return Arrays.stream(storage.get())
                .limit(10)
                .map(row -> Arrays.toString(Arrays.stream(row).limit(10).toArray()))
                .collect(Collectors.joining("\n"));
    }

    public boolean isMatrixExist() {
        return storage.exists();
    }
}
