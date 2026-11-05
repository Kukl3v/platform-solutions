<script>
    import {
        Container,
        Nav,
        NavItem,
        NavLink
    } from 'sveltestrap';

    import RandomTab from './components/RandomTab.svelte';
    import ManualTab from './components/ManualTab.svelte';

    let activeTab = 'random';
</script>

<Container class="py-4">

    <h3 class="mb-4">Среднее арифметическое матрицы</h3>

    <Nav tabs>
        <NavItem>
            <NavLink
                active={activeTab === 'random'}
                on:click={() => activeTab = 'random'}
            >
                Рандом
            </NavLink>
        </NavItem>

        <NavItem>
            <NavLink
                active={activeTab === 'manual'}
                on:click={() => activeTab = 'manual'}
            >
                Заполнение
            </NavLink>
        </NavItem>
    </Nav>

    <div class="border border-secondary-subtle rounded-bottom p-3">

        {#if activeTab === 'random'}
            <RandomTab />
        {:else}
            <ManualTab />
        {/if}

    </div>

</Container>
