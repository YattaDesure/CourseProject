import tkinter as tk
from tkinter import ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 1")
    root.geometry("520x260")

    mode = tk.StringVar(value="letters")  # radiobutton

    ttk.Label(
        root,
        text="Задание 1.\n"
        "Вводи слово и жми кнопку.\n"
        "Для выхода введи 999.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ttk.Label(frm, text="Слово:").pack(side="left")
    ent = ttk.Entry(frm)
    ent.pack(side="left", fill="x", expand=True, padx=8)
    ent.focus_set()

    opts = ttk.Frame(root)
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Считать только буквы", variable=mode, value="letters").pack(
        side="left"
    )
    ttk.Radiobutton(opts, text="Считать все символы", variable=mode, value="all").pack(
        side="left", padx=12
    )

    out = tk.Text(root, height=6, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))
    out.configure(state="disabled")

    stopped = {"v": False}

    def append(s: str):
        out.configure(state="normal")
        out.insert("end", s + "\n")
        out.see("end")
        out.configure(state="disabled")

    def calc():
        if stopped["v"]:
            return
        word = ent.get()
        ent.delete(0, "end")

        if word == "999":
            stopped["v"] = True
            ent.configure(state="disabled")
            btn.configure(state="disabled")
            append("Выход: введено 999.")
            return

        if mode.get() == "all":
            n = len(word)
        else:
            n = 0
            for ch in word:
                if ch.isalpha():
                    n += 1

        append(f"{word} -> {n}")

    btn = ttk.Button(root, text="Ок", command=calc)
    btn.pack(anchor="w", padx=10)

    root.bind("<Return>", lambda _e: calc())
    root.mainloop()


if __name__ == "__main__":
    main()

