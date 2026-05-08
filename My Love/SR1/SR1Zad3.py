import math
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №1 — Задание 3")
    root.geometry("680x420")

    ttk.Label(
        root,
        text="Задание 3.\nРавнобедренный прямоугольный треугольник.\n"
        "1 — катет a, 2 — гипотенуза c, 3 — высота h, 4 — площадь S.\n"
        "Дан номер и значение — найти все 4 величины.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    num = tk.StringVar(value="1")  # radiobutton
    opts = ttk.LabelFrame(root, text="Какой элемент дан")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="1 — катет a", variable=num, value="1").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="2 — гипотенуза c", variable=num, value="2").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="3 — высота h", variable=num, value="3").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="4 — площадь S", variable=num, value="4").pack(
        side="left", padx=8, pady=6
    )

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="Значение:").pack(side="left")
    ent = ttk.Entry(row, width=14)
    ent.pack(side="left", padx=8)
    ent.focus_set()

    out = tk.Text(root, height=8, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        try:
            v = float(ent.get())
        except Exception:
            messagebox.showerror("Ошибка", "Введи число.")
            return
        if v <= 0:
            messagebox.showerror("Ошибка", "Значение должно быть > 0.")
            return

        n = num.get()
        # формулы для равнобедренного прямоугольного треугольника:
        # c = a * sqrt(2)
        # h = a / sqrt(2)
        # S = a*a / 2
        if n == "1":
            a = v
        elif n == "2":
            a = v / math.sqrt(2)
        elif n == "3":
            a = v * math.sqrt(2)
        else:
            a = math.sqrt(2 * v)

        c = a * math.sqrt(2)
        h = a / math.sqrt(2)
        s = a * a / 2

        out.delete("1.0", "end")
        out.insert("end", f"a (катет)         = {a:.6g}\n")
        out.insert("end", f"c (гипотенуза)    = {c:.6g}\n")
        out.insert("end", f"h (высота)        = {h:.6g}\n")
        out.insert("end", f"S (площадь)       = {s:.6g}\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
