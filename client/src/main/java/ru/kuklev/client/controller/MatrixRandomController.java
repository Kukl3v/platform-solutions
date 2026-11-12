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
import ru.kuklev.client.dto.MatrixResponse;
import ru.kuklev.client.model.RandomMatrixForm;
import ru.kuklev.client.validation.MatrixCalculation;

@Controller
public class MatrixRandomController {
    private final MatrixClient matrixClient;

    private static final Logger LOGGER = LoggerFactory.getLogger(MatrixRandomController.class);

    public MatrixRandomController(MatrixClient matrixClient) {
        this.matrixClient = matrixClient;
    }

    @GetMapping("/")
    public String home() {
        return "redirect:/random";
    }

    @GetMapping("/random")
    public String page(@Valid @ModelAttribute("form") RandomMatrixForm form,
                       BindingResult bindingResult) {
        return "random";
    }

    @PostMapping("/random/generate")
    public String generate(@Valid @ModelAttribute("form") RandomMatrixForm form,
                           BindingResult bindingResult) {
        if (bindingResult.hasErrors()) {
            return "random";
        }

        try {
            MatrixResponse response = matrixClient.getMatrix(
                    form.getRows(), form.getCols(), form.getMinNum(), form.getMaxNum()
            );
            LOGGER.info("[SERVER RESPONSE] getMatrix: {}", response);
            form.setMatrix(response.matrix());
        } catch (RestClientResponseException exception) {
            MatrixControllerSupport.addServerErrors(exception, bindingResult);
        } catch (RestClientException exception) {
            MatrixControllerSupport.addConnectionError(exception, bindingResult);
        }
        return "random";
    }

    @PostMapping("/random/average")
    public String calculateAverage(@Validated(MatrixCalculation.class)
                                   @ModelAttribute("form") RandomMatrixForm form,
                                   BindingResult bindingResult,
                                   Model model) {
        if (bindingResult.hasErrors()) {
            return "random";
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
        return "random";
    }

}
