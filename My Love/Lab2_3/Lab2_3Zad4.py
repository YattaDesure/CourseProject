import random
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("ЛР 2_3, вариант 8 — Задание 4")
    root.geometry("950x560")

    ttk.Label(
        root,
        text="Задание 4.\n30 пунктов на прямой.\nНайти место станции, чтобы сумма расстояний была минимальна (медиана).\n"
        "Показать на рисунке.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    mode = tk.StringVar(value="rnd")  # radiobutton
    opts = ttk.LabelFrame(root, text="Точки")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Случайные 30", variable=mode, value="rnd").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Ввести свои", variable=mode, value="my").pack(
        side="left", padx=8, pady=6
    )

    ttk.Label(root, text="Если свои: 30 чисел (координаты) через пробел:").pack(anchor="w", padx=10)
    ent = ttk.Entry(root)
    ent.pack(fill="x", padx=10, pady=(4, 8))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    res = ttk.Label(row, text="Станция: ")
    res.pack(side="left")
    ttk.Button(row, text="Ок", command=lambda: run()).pack(side="right")

    canv = tk.Canvas(root, bg="white", height=360)
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def median(xs):
        xs = sorted(xs)
        n = len(xs)
        if n % 2 == 1:
            return xs[n // 2]
        return (xs[n // 2 - 1] + xs[n // 2]) / 2

    def run():
        if mode.get() == "my":
            parts = ent.get().strip().split()
            if len(parts) != 30:
                messagebox.showerror("Ошибка", "Нужно ввести ровно 30 чисел.")
                return
            try:
                pts = [float(p) for p in parts]
            except Exception:
                messagebox.showerror("Ошибка", "Не получилось прочитать числа.")
                return
        else:
            pts = []
            for _ in range(30):
                pts.append(random.uniform(0, 100))

        st = median(pts)
        s = 0.0
        for x in pts:
            s += abs(x - st)
        res.configure(text=f"Станция: {st:.2f}   Сумма расстояний: {s:.2f}")

        # рисуем
        canv.delete("all")
        w = int(canv.winfo_width() or 900)
        h = int(canv.winfo_height() or 360)
        left = 40
        right = w - 40
        y = h // 2

        mn = min(pts + [st])
        mx = max(pts + [st])
        if mx == mn:
            mx = mn + 1.0

        def x_to_px(x):
            return left + (x - mn) * (right - left) / (mx - mn)

        # линия
        canv.create_line(left, y, right, y, fill="black", width=2)

        # точки
        for x in pts:
            px = x_to_px(x)
            canv.create_oval(px - 3, y - 3, px + 3, y + 3, fill="blue", outline="")

        # станция
        pxs = x_to_px(st)
        canv.create_line(pxs, y - 20, pxs, y + 20, fill="red", width=3)
        canv.create_text(pxs, y - 28, text="СТ", fill="red")

    root.mainloop()


if __name__ == "__main__":
    main()

