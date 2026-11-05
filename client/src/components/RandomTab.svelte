<script>
    import {
        Row,
        Col,
        Form,
        FormGroup,
        Label,
        Input,
        Button,
        Alert,
        Spinner
    } from 'sveltestrap';

    import {
        generateMatrix,
        calculateAverage,
        getFieldErrors
    } from '../lib/matrixApi';

    let rows = 3;
    let cols = 3;
    let minNum = 0;
    let maxNum = 100;

    let matrix = [];
    let average = null;

    let fieldErrors = {};
    let generalError = '';

    let loading = false;
    let generated = false;

    async function generate() {
        fieldErrors = {};
        generalError = '';
        average = null;
        matrix = [];
        generated = false;
        loading = true;

        try {
            const response = await generateMatrix({
                rows,
                cols,
                minNum,
                maxNum
            });

            matrix = response.matrix;
            generated = true;

        } catch (error) {
            fieldErrors = getFieldErrors(error);
            generalError = error.errors?.length
                ? ''
                : error.message;
        } finally {
            loading = false;
        }
    }

    async function findAverage() {
        fieldErrors = {};
        generalError = '';
        average = null;
        loading = true;

        try {
            const response = await calculateAverage(matrix);
            average = response.average;

        } catch (error) {
            fieldErrors = getFieldErrors(error);
            generalError = error.errors?.length
                ? ''
                : error.message;
        } finally {
            loading = false;
        }
    }
</script>

<form onsubmit={(event) => {
          event.preventDefault();
          generate();
      }}>

    <Row class="g-3 align-items-end">

        <Col xs="6" md="2">
            <FormGroup>
                <Label>Строки</Label>
                <Input
                    type="number"
                    bind:value={rows}
                    min="1"
                    max="100"
                    invalid={!!fieldErrors.rows}
                />
                {#if fieldErrors.rows}
                    <div class="text-danger small mt-1">
                        {fieldErrors.rows}
                    </div>
                {/if}
            </FormGroup>
        </Col>

        <Col xs="6" md="2">
            <FormGroup>
                <Label>Колонки</Label>
                <Input
                    type="number"
                    bind:value={cols}
                    min="1"
                    max="100"
                    invalid={!!fieldErrors.cols}
                />
                {#if fieldErrors.cols}
                    <div class="text-danger small mt-1">
                        {fieldErrors.cols}
                    </div>
                {/if}
            </FormGroup>
        </Col>

        <Col xs="6" md="2">
            <FormGroup>
                <Label>Минимальное число</Label>
                <Input
                    type="number"
                    bind:value={minNum}
                    invalid={!!fieldErrors.minNum}
                />
                {#if fieldErrors.minNum}
                    <div class="text-danger small mt-1">
                        {fieldErrors.minNum}
                    </div>
                {/if}
            </FormGroup>
        </Col>

        <Col xs="6" md="2">
            <FormGroup>
                <Label>Максимальное число</Label>
                <Input
                    type="number"
                    bind:value={maxNum}
                    invalid={!!fieldErrors.maxNum}
                />
                {#if fieldErrors.maxNum}
                    <div class="text-danger small mt-1">
                        {fieldErrors.maxNum}
                    </div>
                {/if}
            </FormGroup>
        </Col>

        <Col xs="12" md="2">
            <div class="d-grid">
                <Button
                    color="primary"
                    type="submit"
                    disabled={loading}
                >
                    {#if loading}
                        <Spinner size="sm" />
                    {:else}
                        Сгенерировать
                    {/if}
                </Button>
            </div>
        </Col>

        <Col xs="12" md="2">
            <div class="d-grid">
                {#if generated}
                    <Button
                        color="success"
                        outline
                        disabled={loading}
                        on:click={findAverage}
                    >
                        Найти среднее
                    </Button>
                {/if}
            </div>
        </Col>

    </Row>

</form>

{#if fieldErrors.validRange}
    <Alert color="danger" class="mt-3 mb-0">
        {fieldErrors.validRange}
    </Alert>
{/if}

{#if generalError}
    <Alert color="danger" class="mt-3 mb-0">
        {generalError}
    </Alert>
{/if}

{#if average !== null}
    <Alert color="success" class="mt-3 mb-0">
        **Среднее арифметическое:** {average}
    </Alert>
{/if}

{#if generated}

    <hr />

    <h5 class="mb-3">Матрица</h5>

    <div style="overflow-x: auto; max-width: 100%;">
        <div class="d-flex flex-column gap-2" style="width: max-content;">

            {#each matrix as row, i}
                <div class="d-flex flex-wrap gap-2" >
                    {#each row as value, j}
                        <div style="width: 85px;">
                            <Input
                                type="number"
                                bind:value={matrix[i][j]}
                                disabled={loading}
                            />
                        </div>
                    {/each}
                </div>
            {/each}

        </div>
    </div>

{/if}
