package ru.kuklev.monolith.controller;

import jakarta.validation.Valid;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PostMapping;
import ru.kuklev.monolith.model.RandomMatrixForm;
import ru.kuklev.monolith.service.MatrixService;
import ru.kuklev.monolith.validation.MatrixCalculation;

@Controller
public class MatrixRandomController {
    private final MatrixService matrixService;

    public MatrixRandomController(MatrixService matrixService) {
        this.matrixService = matrixService;
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
        if (!bindingResult.hasErrors()) {
            form.setMatrix(matrixService.generateMatrix(
                    form.getRows(), form.getCols(), form.getMinNum(), form.getMaxNum()
            ));
        }
        return "random";
    }

    @PostMapping("/random/average")
    public String calculateAverage(@Validated(MatrixCalculation.class)
                                   @ModelAttribute("form") RandomMatrixForm form,
                                   BindingResult bindingResult,
                                   Model model) {
        if (!bindingResult.hasErrors()) {
            model.addAttribute("average", matrixService.calculateMatrixAverage(form.getMatrix()));
        }
        return "random";
    }
}
