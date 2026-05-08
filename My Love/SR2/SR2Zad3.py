import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №2 — Задание 3 (домики в перспективе)")
    root.geometry("900x560")

    ttk.Label(
        root,
        text="Задание 3.\nN домиков, уходящих вдаль.\nИз левого нижнего (X,Y) в правый верхний угол.\nПервый домик — сторона A.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    defaults = {"N": "5", "X": "40", "Y": "440", "A": "150"}
    for i, name in enumerate(("N", "X", "Y", "A")):
        ttk.Label(frm, text=name + ":").grid(row=0, column=i * 2, padx=4, sticky="w")
        e = ttk.Entry(frm, width=8)
        e.grid(row=0, column=i * 2 + 1, padx=4, sticky="w")
        e.insert(0, defaults[name])
        ents[name] = e

    color = tk.StringVar(value="#f5deb3")  # radiobutton
    opts = ttk.LabelFrame(root, text="Цвет дома")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Бежевый", variable=color, value="#f5deb3").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Розовый", variable=color, value="#ffb6c1").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Зелёный", variable=color, value="#90ee90").pack(
        side="left", padx=8, pady=6
    )

    canv = tk.Canvas(root, bg="white")
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def draw_house(x, y, a, c):
        # x, y — левый нижний угол основания
        canv.create_rectangle(x, y - a, x + a, y, fill=c, outline="black", width=2)
        # крыша
        canv.create_polygon(
            x, y - a, x + a, y - a, x + a // 2, y - a - a // 2,
            fill="#a0522d", outline="black", width=2,
        )
        # дверь
        d_w = a // 5
        d_h = a // 2
        canv.create_rectangle(
            x + a // 2 - d_w // 2, y - d_h, x + a // 2 + d_w // 2, y,
            fill="#4b3621", outline="black", width=2,
        )
        # окошко
        canv.create_rectangle(
            x + a // 6, y - 3 * a // 4, x + a // 6 + a // 5, y - 3 * a // 4 + a // 5,
            fill="lightblue", outline="black", width=2,
        )

    def run():
        try:
            n = int(ents["N"].get())
            x = int(ents["X"].get())
            y = int(ents["Y"].get())
            a = int(ents["A"].get())
        except Exception:
            messagebox.showerror("Ошибка", "N, X, Y, A — целые числа.")
            return
        if n <= 0 or a <= 0:
            messagebox.showerror("Ошибка", "N и A должны быть > 0.")
            return

        canv.delete("all")
        # рисуем от дальнего к ближнему, чтобы ближние перекрывали
        w = int(canv.winfo_width() or 880)
        h = int(canv.winfo_height() or 440)
        # конечная точка — правый верхний угол
        end_x = w - 20
        end_y = 20

        for i in range(n - 1, -1, -1):
            t = i / max(n - 1, 1)  # 0..1, 0 — ближний, 1 — дальний
            sz = max(8, int(a * (1 - 0.85 * t)))
            cx = int(x + (end_x - x) * t)
            cy = int(y + (end_y - y) * t)
            draw_house(cx, cy, sz, color.get())

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
