package ru.kuklev.server.validation;

import jakarta.servlet.http.HttpServletRequest;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.core.Ordered;
import org.springframework.core.annotation.Order;
import org.springframework.http.HttpStatus;
import org.springframework.http.ProblemDetail;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.List;

@Order(Ordered.HIGHEST_PRECEDENCE)
@RestControllerAdvice
public class AdviceController {
    private static final Logger LOGGER = LoggerFactory.getLogger(AdviceController.class);

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ProblemDetail handleValidationException(MethodArgumentNotValidException e,
                                                   HttpServletRequest request) {

        ProblemDetail problem = ProblemDetail.forStatusAndDetail(
                HttpStatus.BAD_REQUEST,
                "Validation error"
        );

        problem.setTitle("Validation Failed");

        List<ValidationError> errors = e.getBindingResult()
                .getAllErrors()
                .stream()
                .map(error -> new ValidationError(
                        error instanceof FieldError fieldError
                                ? fieldError.getField()
                                : error.getObjectName(),
                        error.getDefaultMessage()
                ))
                .toList();

        problem.setProperty("errors", errors);

        LOGGER.error("API error: [{}, instance={}, status={}, title={} errors={}]",
                problem.getDetail(),
                request.getRequestURI(),
                problem.getStatus(),
                problem.getTitle(),
                errors);

        return problem;
    }
}
