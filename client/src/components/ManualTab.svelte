<script>
    import {
        Row,
        Col,
        FormGroup,
        Label,
        Input,
        Button,
        Alert,
        Spinner
    } from 'sveltestrap';

    import {
        calculateAverage,
        getFieldErrors
    } from '../lib/matrixApi';

    let rows = 3;
    let cols = 3;

    let matrix = createMatrix(rows, cols);
    let average = null;

    let fieldErrors = {};
    let generalError = '';
    let loading = false;

    function createMatrix(r, c, oldMatrix = []) {
        return Array.from({ length: r }, (_, i) =>
            Array.from({ length: c }, (_, j) =>
                oldMatrix[i]?.[j] ?? undefined
            )
        );
    }

    function resizeMatrix() {
        const r = Math.min(100, Math.max(1, Number(rows) || 1));
        const c = Math.min(100, Math.max(1, Number(cols) || 1));

        matrix = createMatrix(r, c, matrix);
        average = null;
        fieldErrors = {};
        generalError = '';
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

<Row class="g-3 align-items-end">

    <Col xs="6" md="3">
        <FormGroup>
            <Label>Строки</Label>
            <Input
                type="number"
                bind:value={rows}
                min="1"
                max="100"
                disabled={loading}
                on:change={resizeMatrix}
                invalid={!!fieldErrors.rows}
            />
            {#if fieldErrors.rows}
                <div class="text-danger small mt-1">
                    {fieldErrors.rows}
                </div>
            {/if}
        </FormGroup>
    </Col>

    <Col xs="6" md="3">
        <FormGroup>
            <Label>Колонки</Label>
            <Input
                type="number"
                bind:value={cols}
                min="1"
                max="100"
                disabled={loading}
                on:change={resizeMatrix}
                invalid={!!fieldErrors.cols}
            />
            {#if fieldErrors.cols}
                <div class="text-danger small mt-1">
                    {fieldErrors.cols}
                </div>
            {/if}
        </FormGroup>
    </Col>

    <Col xs="12" md="3">
        <div class="d-grid">
            <Button
                color="success"
                disabled={loading}
                on:click={findAverage}
            >
                {#if loading}
                    <Spinner size="sm" />
                {:else}
                    Найти среднее
                {/if}
            </Button>
        </div>
    </Col>

</Row>

{#if generalError}
    <Alert color="danger" class="mt-3 mb-0">
        {generalError}
    </Alert>
{/if}

{#if fieldErrors.matrix}
    <Alert color="danger" class="mt-3 mb-0">
        {fieldErrors.matrix}
    </Alert>
{/if}

{#if average !== null}
    <Alert color="success" class="mt-3 mb-0">
        **Среднее арифметическое:** {average}
    </Alert>
{/if}

<hr />

<h5 class="mb-3">Матрица</h5>

<div style="overflow-x: auto; max-width: 100%;">
        <div class="d-flex flex-column gap-2" style="width: max-content;">

        {#each matrix as row, i}
            <div class="d-flex flex-wrap gap-2">
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
