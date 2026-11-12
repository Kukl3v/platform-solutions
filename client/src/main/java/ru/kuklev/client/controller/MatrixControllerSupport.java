package ru.kuklev.client.controller;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.validation.BindingResult;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestClientResponseException;
import ru.kuklev.client.dto.error.ProblemResponse;
import ru.kuklev.client.dto.error.ValidationError;

import java.util.Set;

final class MatrixControllerSupport {
    private static final Set<String> FORM_FIELDS = Set.of(
            "rows", "cols", "minNum", "maxNum", "matrix"
    );

    private static final Logger LOGGER = LoggerFactory.getLogger(MatrixControllerSupport.class);

    private MatrixControllerSupport() {
    }

    static void addServerErrors(RestClientResponseException exception, BindingResult bindingResult) {
        LOGGER.error("[SERVER ERROR]: {}", exception.getMessage());
        ProblemResponse problem = readProblem(exception);
        if (problem != null && problem.errors() != null && !problem.errors().isEmpty()) {
            for (ValidationError error : problem.errors()) {
                String field = normalizeField(error.field());
                if (field == null) {
                    bindingResult.reject("server.validation", error.message());
                } else {
                    bindingResult.rejectValue(field, "server.validation", error.message());
                }
            }
            return;
        }

        String message = problem != null && problem.detail() != null
                ? problem.detail()
                : "Сервер отклонил запрос";
        bindingResult.reject("server.error", message);
    }

    static void addConnectionError(RestClientException exception, BindingResult bindingResult) {
        LOGGER.error("[CONNECTION ERROR]: {}", exception.getMessage());
        bindingResult.reject("server.unavailable", "Не удалось получить ответ от сервера");
    }

    private static ProblemResponse readProblem(RestClientResponseException exception) {
        try {
            return exception.getResponseBodyAs(ProblemResponse.class);
        } catch (RuntimeException ignored) {
            return null;
        }
    }

    private static String normalizeField(String field) {
        if ("validRange".equals(field)) {
            return "minNum";
        }
        return FORM_FIELDS.contains(field) ? field : null;
    }
}
