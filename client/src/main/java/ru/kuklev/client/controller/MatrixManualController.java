package ru.kuklev.client.controller;

import jakarta.validation.Valid;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestClientResponseException;
import ru.kuklev.client.client.MatrixClient;
import ru.kuklev.client.dto.MatrixAverageResponse;
import ru.kuklev.client.dto.MatrixCalculateRequest;
import ru.kuklev.client.model.ManualMatrixForm;
import ru.kuklev.client.validation.MatrixCalculation;

@Controller
public class MatrixManualController {
    private final MatrixClient matrixClient;

    private static final Logger LOGGER = LoggerFactory.getLogger(MatrixManualController.class);

    public MatrixManualController(MatrixClient matrixClient) {
        this.matrixClient = matrixClient;
    }

    @GetMapping("/manual")
    public String page(@Valid @ModelAttribute("form") ManualMatrixForm form,
                       BindingResult bindingResult) {
        if (!bindingResult.hasErrors()) {
            form.setMatrix(new int[form.getRows()][form.getCols()]);
        }
        return "manual";
    }

    @PostMapping("/manual/resize")
    public String resize(@Valid @ModelAttribute("form") ManualMatrixForm form,
                         BindingResult bindingResult) {
        if (!bindingResult.hasErrors()) {
            form.setMatrix(resizeMatrix(form.getMatrix(), form.getRows(), form.getCols()));
        }
        return "manual";
    }

    @PostMapping("/manual/average")
    public String calculateAverage(@Validated(MatrixCalculation.class)
                                   @ModelAttribute("form") ManualMatrixForm form,
                                   BindingResult bindingResult,
                                   Model model) {
        if (bindingResult.hasErrors()) {
            return "manual";
        }

        try {
            MatrixAverageResponse response = matrixClient.calculateAverage(
                    new MatrixCalculateRequest(form.getMatrix())
            );
            LOGGER.info("[SERVER RESPONSE] calculateAverage: {}", response);
            model.addAttribute("average", response.average());
        } catch (RestClientResponseException exception) {
            MatrixControllerSupport.addServerErrors(exception, bindingResult);
        } catch (RestClientException exception) {
            MatrixControllerSupport.addConnectionError(exception, bindingResult);
        }
        return "manual";
    }

    private int[][] resizeMatrix(int[][] source, int rows, int cols) {
        int[][] resized = new int[rows][cols];
        if (source == null) {
            return resized;
        }
        for (int row = 0; row < Math.min(rows, source.length); row++) {
            if (source[row] != null) {
                System.arraycopy(source[row], 0, resized[row], 0, Math.min(cols, source[row].length));
            }
        }
        return resized;
    }
}
