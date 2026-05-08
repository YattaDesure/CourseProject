import tkinter as tk
from tkinter import ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 4")
    root.geometry("620x260")

    mode = tk.StringVar(value="all")  # radiobutton

    ttk.Label(
        root,
        text="Задание 4.\nПосчитать заглавные буквы в строке.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    ttk.Label(root, text="Строка:").pack(anchor="w", padx=10)
    ent = ttk.Entry(root)
    ent.pack(fill="x", padx=10, pady=(4, 8))
    ent.focus_set()

    opts = ttk.LabelFrame(root, text="Режим")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Все (рус/англ)", variable=mode, value="all").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только A..Z", variable=mode, value="latin").pack(
        side="left", padx=8, pady=6
    )

    res = ttk.Label(root, text="Результат: 0")
    res.pack(anchor="w", padx=10, pady=6)

    def run():
        s = ent.get()
        cnt = 0
        if mode.get() == "latin":
            for ch in s:
                if "A" <= ch <= "Z":
                    cnt += 1
        else:
            for ch in s:
                if ch.isalpha() and ch.isupper():
                    cnt += 1
        res.configure(text=f"Результат: {cnt}")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=6)
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()

