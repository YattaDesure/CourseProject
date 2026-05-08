import math
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №2 — Задание 5")
    root.geometry("680x520")

    ttk.Label(
        root,
        text="Задание 5.\nПосчитать значения функции.\n"
        "Y = sin(x), если x > 16\n"
        "Y = sqrt(25 - x^2), если x <= 9\n"
        "При 9 < x <= 16 функция не определена.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ttk.Label(frm, text="x_min:").grid(row=0, column=0, sticky="w", padx=4)
    ent_min = ttk.Entry(frm, width=10)
    ent_min.grid(row=0, column=1, sticky="w", padx=4)
    ent_min.insert(0, "-5")

    ttk.Label(frm, text="x_max:").grid(row=0, column=2, sticky="w", padx=4)
    ent_max = ttk.Entry(frm, width=10)
    ent_max.grid(row=0, column=3, sticky="w", padx=4)
    ent_max.insert(0, "20")

    ttk.Label(frm, text="шаг:").grid(row=0, column=4, sticky="w", padx=4)
    ent_step = ttk.Entry(frm, width=10)
    ent_step.grid(row=0, column=5, sticky="w", padx=4)
    ent_step.insert(0, "1")

    mode = tk.StringVar(value="all")  # radiobutton
    opts = ttk.LabelFrame(root, text="Что показывать")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Все x", variable=mode, value="all").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Только где определена", variable=mode, value="def").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Результат (Список):").pack(anchor="w", padx=10)
    lst = tk.Listbox(root, height=14)
    lst.pack(fill="both", expand=True, padx=10, pady=(4, 10))

    def run():
        try:
            xmin = float(ent_min.get())
            xmax = float(ent_max.get())
            step = float(ent_step.get())
        except Exception:
            messagebox.showerror("Ошибка", "x_min, x_max, шаг — числа.")
            return
        if step <= 0:
            messagebox.showerror("Ошибка", "Шаг > 0.")
            return
        if xmax < xmin:
            xmin, xmax = xmax, xmin

        lst.delete(0, "end")
        x = xmin
        # чтобы избежать накопления ошибки, считаем количество шагов
        n = int(round((xmax - xmin) / step)) + 1
        for i in range(n):
            x = xmin + i * step
            if x <= 9:
                if 25 - x * x < 0:
                    if mode.get() == "all":
                        lst.insert("end", f"x = {x:g}   y = не определена")
                    continue
                y = math.sqrt(25 - x * x)
                lst.insert("end", f"x = {x:g}   y = {y:.6g}")
            elif x > 16:
                y = math.sin(x)
                lst.insert("end", f"x = {x:g}   y = {y:.6g}")
            else:
                if mode.get() == "all":
                    lst.insert("end", f"x = {x:g}   y = не определена")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
