import math
import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №1 — Задание 4")
    root.geometry("760x600")

    ttk.Label(
        root,
        text="Задание 4.\nГрафик в полярных координатах: R^2 = A^2 * cos(N*α).\n"
        "Перевод: X = K*R*cos(α), Y = K*R*sin(α).",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    defaults = {"A": "100", "N": "2", "K": "1"}
    for i, name in enumerate(("A", "N", "K")):
        ttk.Label(frm, text=name + ":").grid(row=0, column=i * 2, padx=4, pady=2, sticky="w")
        e = ttk.Entry(frm, width=8)
        e.grid(row=0, column=i * 2 + 1, padx=4, pady=2, sticky="w")
        e.insert(0, defaults[name])
        ents[name] = e

    color = tk.StringVar(value="blue")  # radiobutton
    opts = ttk.LabelFrame(root, text="Цвет")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Синий", variable=color, value="blue").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Красный", variable=color, value="red").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Зелёный", variable=color, value="green").pack(
        side="left", padx=8, pady=6
    )

    canv = tk.Canvas(root, bg="white", height=420)
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        try:
            A = float(ents["A"].get())
            N = float(ents["N"].get())
            K = float(ents["K"].get())
        except Exception:
            messagebox.showerror("Ошибка", "A, N, K должны быть числами.")
            return

        canv.delete("all")
        w = int(canv.winfo_width() or 700)
        h = int(canv.winfo_height() or 420)
        cx = w // 2
        cy = h // 2

        # оси
        canv.create_line(0, cy, w, cy, fill="#cccccc")
        canv.create_line(cx, 0, cx, h, fill="#cccccc")

        pts = []
        steps = 2000
        for i in range(steps + 1):
            alpha = -math.pi + 2 * math.pi * i / steps
            r2 = A * A * math.cos(N * alpha)
            if r2 < 0:
                pts.append(None)
                continue
            r = math.sqrt(r2)
            x = K * r * math.cos(alpha)
            y = K * r * math.sin(alpha)
            pts.append((cx + x, cy - y))

        prev = None
        for p in pts:
            if p is None:
                prev = None
                continue
            if prev is not None:
                canv.create_line(prev[0], prev[1], p[0], p[1], fill=color.get(), width=2)
            prev = p

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
