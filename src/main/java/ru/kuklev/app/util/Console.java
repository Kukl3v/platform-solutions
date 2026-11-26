package ru.kuklev.app.util;

import org.jline.terminal.Terminal;
import org.jline.utils.AttributedString;
import org.jline.utils.AttributedStyle;
import org.jline.utils.InfoCmp;
import org.springframework.shell.jline.tui.component.flow.ComponentFlow.Builder;
import org.springframework.stereotype.Component;

@Component
public class Console {

    private final Terminal terminal;
    private final Builder flowBuilder;

    public Console(Terminal terminal,
                   Builder flowBuilder) {
        this.terminal = terminal;
        this.flowBuilder = flowBuilder;
    }

    public void print(String text) {
        terminal.writer().println(text);
        terminal.flush();
    }

    public void success(String text) {
        printStyled(String.format("[SUCCESS] %s", text), AttributedStyle.GREEN);
    }

    public void error(String text) {
        printStyled(String.format("[ERROR] %s", text), AttributedStyle.RED);
    }

    public void warning(String text) {
        printStyled(String.format("[WARNING] %s", text), AttributedStyle.YELLOW);
    }

    private void printStyled(String text, int color) {
        print(new AttributedString(
                text,
                AttributedStyle.DEFAULT.foreground(color)
        ).toAnsi(terminal));
    }

    public void clear() {
        terminal.puts(InfoCmp.Capability.clear_screen);
        terminal.flush();
    }

    public void back() {
        flowBuilder.clone()
                .reset()
                .withSingleItemSelector("back")
                    .selectItem("Вернуться назад", "back")
                    .and()
                .build()
                .run();
    }
}
