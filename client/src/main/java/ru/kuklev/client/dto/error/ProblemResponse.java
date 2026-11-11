package ru.kuklev.client.dto.error;

import java.net.URI;
import java.util.List;

public record ProblemResponse(
        String type,
        String title,
        int status,
        String detail,
        URI instance,
        List<ValidationError> errors
) {
}
