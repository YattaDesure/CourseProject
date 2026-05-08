import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №1 — Задание 1")
    root.geometry("520x340")

    ttk.Label(
        root,
        text="Задание 1.\nНайти значение функции y = a*x^2 + b*x + c.\nВведи a, b, c, x.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    for i, name in enumerate(("a", "b", "c", "x")):
        ttk.Label(frm, text=name + ":").grid(row=i, column=0, sticky="w", padx=4, pady=2)
        e = ttk.Entry(frm, width=14)
        e.grid(row=i, column=1, sticky="w", padx=4, pady=2)
        ents[name] = e

    mode = tk.StringVar(value="full")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что выводить")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Полный ответ", variable=mode, value="full").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только y", variable=mode, value="short").pack(
        side="left", padx=8, pady=6
    )

    res = ttk.Label(root, text="Ответ: ")
    res.pack(anchor="w", padx=10, pady=8)

    def run():
        try:
            a = float(ents["a"].get())
            b = float(ents["b"].get())
            c = float(ents["c"].get())
            x = float(ents["x"].get())
        except Exception:
            messagebox.showerror("Ошибка", "Введи числа во все поля.")
            return

        y = a * x * x + b * x + c

        if mode.get() == "short":
            res.configure(text=f"Ответ: y = {y:g}")
        else:
            res.configure(text=f"Ответ: y = a*x^2 + b*x + c = {y:g}")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=4)
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
