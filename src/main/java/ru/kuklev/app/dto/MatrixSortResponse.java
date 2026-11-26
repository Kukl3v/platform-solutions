package ru.kuklev.app.dto;

import java.time.Duration;

public record MatrixSortResponse(String partialMatrix,
                                 Duration time) {
}
