package ru.kuklev.monolith.controller;

import jakarta.validation.Valid;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PostMapping;
import ru.kuklev.monolith.model.ManualMatrixForm;
import ru.kuklev.monolith.service.MatrixService;
import ru.kuklev.monolith.validation.MatrixCalculation;

@Controller
public class MatrixManualController {
    private final MatrixService matrixService;

    public MatrixManualController(MatrixService matrixService) {
        this.matrixService = matrixService;
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
        if (!bindingResult.hasErrors()) {
            model.addAttribute("average", matrixService.calculateMatrixAverage(form.getMatrix()));
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
