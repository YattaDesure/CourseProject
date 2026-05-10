import math
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №1 — Задание 5")
    root.geometry("620x320")

    ttk.Label(
        root,
        text="Задание 5.\nДано N > 0.\nНайти сумму 2 + 1/2! + 1/3! + ... + 1/N!\n(приближение к e ≈ 2.71828183).",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="N:").pack(side="left")
    ent = ttk.Entry(row, width=10)
    ent.pack(side="left", padx=8)
    ent.insert(0, "10")
    ent.focus_set()

    mode = tk.StringVar(value="full")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что выводить")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Сумма и разница с e", variable=mode, value="full").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только сумма", variable=mode, value="short").pack(
        side="left", padx=8, pady=6
    )

    res = ttk.Label(root, text="Ответ: ")
    res.pack(anchor="w", padx=10, pady=8)

    def run():
        try:
            n = int(ent.get())
        except Exception:
            messagebox.showerror("Ошибка", "N должно быть целым.")
            return
        if n <= 0:
            messagebox.showerror("Ошибка", "N должно быть > 0.")
            return

        s = 2.0  # как написано в задании: "2 + 1/2! + 1/3! + ... + 1/N!"
        f = 1
        for k in range(2, n + 1):
            f *= k
            s += 1.0 / f

        if mode.get() == "short":
            res.configure(text=f"Ответ: сумма = {s:.10f}")
        else:
            res.configure(
                text=f"Ответ: сумма = {s:.10f}\n"
                f"e ≈ {math.e:.10f}\nразница: {abs(math.e - s):.10f}"
            )

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=4)
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
