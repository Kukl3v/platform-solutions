package ru.kuklev.server.validation;

public record ValidationError(
        String field,
        String message
) {}
