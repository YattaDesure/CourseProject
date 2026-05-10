import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №2 — Задание 2 (стена из башен)")
    root.geometry("900x560")

    ttk.Label(
        root,
        text="Задание 2.\nСтена из N башен.\nКаждая — три зубца и арка.\n(X,Y) — левый нижний угол. A — длина основания башни.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    defaults = {"N": "4", "X": "60", "Y": "440", "A": "120"}
    for i, name in enumerate(("N", "X", "Y", "A")):
        ttk.Label(frm, text=name + ":").grid(row=0, column=i * 2, padx=4, sticky="w")
        e = ttk.Entry(frm, width=8)
        e.grid(row=0, column=i * 2 + 1, padx=4, sticky="w")
        e.insert(0, defaults[name])
        ents[name] = e

    color = tk.StringVar(value="gray")  # radiobutton
    opts = ttk.LabelFrame(root, text="Цвет стены")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Серый", variable=color, value="gray").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Коричневый", variable=color, value="#8B4513").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Синий", variable=color, value="#3a6ea5").pack(
        side="left", padx=8, pady=6
    )

    canv = tk.Canvas(root, bg="white")
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def draw_tower(x, y, a, c):
        h = 2 * a  # высота башни
        # тело
        canv.create_rectangle(x, y - h, x + a, y, fill=c, outline="black", width=2)
        # три зубца сверху
        tooth_w = a // 5
        gap = (a - 3 * tooth_w) // 4
        tx = x + gap
        for _ in range(3):
            canv.create_rectangle(
                tx, y - h - tooth_w, tx + tooth_w, y - h, fill=c, outline="black", width=2
            )
            tx += tooth_w + gap
        # арка (полукруг сверху проёма)
        ax1 = x + a // 4
        ax2 = x + 3 * a // 4
        ay1 = y - a // 2
        ay2 = y
        canv.create_arc(
            ax1, ay1 - a // 4, ax2, ay1 + a // 4,
            start=0, extent=180, style="pieslice", fill="white", outline="black", width=2,
        )
        canv.create_rectangle(ax1, ay1, ax2, ay2, fill="white", outline="black", width=2)

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
        cx = x
        for _ in range(n):
            draw_tower(cx, y, a, color.get())
            cx += a

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
