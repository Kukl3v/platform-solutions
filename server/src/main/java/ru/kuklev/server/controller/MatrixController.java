package ru.kuklev.server.controller;

import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;
import ru.kuklev.server.dto.MatrixAverageResponse;
import ru.kuklev.server.dto.MatrixCalculateRequest;
import ru.kuklev.server.dto.MatrixGenerateRequest;
import ru.kuklev.server.dto.MatrixResponse;
import ru.kuklev.server.service.MatrixService;

@RestController
@RequestMapping("/api/matrix")
public class MatrixController {
    private final MatrixService matrixService;

    public MatrixController(MatrixService matrixService) {
        this.matrixService = matrixService;
    }

    @GetMapping
    public MatrixResponse getMatrix(@Valid @ModelAttribute MatrixGenerateRequest request) {
        return matrixService.generateMatrix(request);
    }

    @PostMapping("/calculate/average")
    public MatrixAverageResponse calculateAverage(@Valid @RequestBody MatrixCalculateRequest request) {
        return matrixService.calculateMatrixAverage(request);
    }
}
