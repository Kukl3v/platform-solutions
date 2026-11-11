package ru.kuklev.client.dto.error;

public record ValidationError(
        String field,
        String message
) {}
