package ru.kuklev.client.client;

import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.service.annotation.GetExchange;
import org.springframework.web.service.annotation.HttpExchange;
import org.springframework.web.service.annotation.PostExchange;
import ru.kuklev.client.dto.MatrixAverageResponse;
import ru.kuklev.client.dto.MatrixCalculateRequest;
import ru.kuklev.client.dto.MatrixResponse;

@HttpExchange("/api/matrix")
public interface MatrixClient {

    @GetExchange
    MatrixResponse getMatrix(@RequestParam int rows,
                             @RequestParam int cols,
                             @RequestParam int minNum,
                             @RequestParam int maxNum);

    @PostExchange("/calculate/average")
    MatrixAverageResponse calculateAverage(@RequestBody MatrixCalculateRequest request);
}
