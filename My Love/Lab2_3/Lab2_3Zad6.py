import random
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 6")
    root.geometry("860x420")

    ttk.Label(
        root,
        text="Задание 6.\nПодсчитать, сколько элементов массива удовлетворяют i*i <= a[i].\n"
        "i считаем с 1 (первый элемент).",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    mode = tk.StringVar(value="rnd")  # radiobutton
    opts = ttk.LabelFrame(root, text="Массив")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Случайный (20 чисел)", variable=mode, value="rnd").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Ввести свой", variable=mode, value="my").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Если свой: числа через пробел:").pack(anchor="w", padx=10)
    ent = ttk.Entry(root)
    ent.pack(fill="x", padx=10, pady=(4, 8))

    out = tk.Text(root, height=12, wrap="word")
    out.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        if mode.get() == "my":
            parts = ent.get().strip().split()
            if not parts:
                messagebox.showerror("Ошибка", "Введите массив.")
                return
            try:
                a = [float(p) for p in parts]
            except Exception:
                messagebox.showerror("Ошибка", "Не получилось прочитать числа.")
                return
        else:
            a = []
            for _ in range(20):
                a.append(random.uniform(0, 200))

        cnt = 0
        good = []
        for i in range(len(a)):
            idx = i + 1  # i с 1
            if idx * idx <= a[i]:
                cnt += 1
                good.append(idx)

        out.delete("1.0", "end")
        out.insert("end", "Массив:\n")
        out.insert("end", " ".join(f"{x:.2f}" for x in a) + "\n\n")
        out.insert("end", f"Ответ: {cnt}\n")
        if good:
            out.insert("end", "Подходят позиции: " + ", ".join(map(str, good)) + "\n")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()

