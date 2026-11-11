package ru.kuklev.client.model;

import jakarta.validation.ConstraintViolation;
import jakarta.validation.Validator;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.validation.BeanPropertyBindingResult;
import org.springframework.validation.SmartValidator;
import ru.kuklev.client.validation.MatrixCalculation;

import java.util.Set;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest
class MatrixFormValidationTest {
    @Autowired
    private Validator validator;

    @Autowired
    private SmartValidator smartValidator;

    @Test
    void manualForm_ShouldRequireMatrixForCalculation() {
        ManualMatrixForm form = new ManualMatrixForm();

        Set<ConstraintViolation<ManualMatrixForm>> violations =
                validator.validate(form, MatrixCalculation.class);

        assertThat(violations)
                .extracting(violation -> violation.getPropertyPath().toString())
                .contains("matrix");
    }

    @Test
    void randomForm_ShouldValidateNumberRange() {
        RandomMatrixForm form = new RandomMatrixForm();
        form.setMinNum(10);
        form.setMaxNum(1);

        Set<ConstraintViolation<RandomMatrixForm>> violations = validator.validate(form);

        assertThat(violations)
                .extracting(violation -> violation.getPropertyPath().toString())
                .contains("validRange");
    }

    @Test
    void smartValidator_ShouldApplyCalculationGroup() {
        ManualMatrixForm form = new ManualMatrixForm();
        BeanPropertyBindingResult bindingResult = new BeanPropertyBindingResult(form, "form");

        smartValidator.validate(form, bindingResult, MatrixCalculation.class);

        assertThat(bindingResult.hasFieldErrors("matrix")).isTrue();
    }
}
