import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №2 — Задание 1 (конфетки)")
    root.geometry("900x520")

    ttk.Label(
        root,
        text="Задание 1.\nНарисовать N конфеток, начиная от (X,Y) влево.\nСторона конфетки A.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    defaults = {"N": "5", "X": "800", "Y": "260", "A": "60"}
    for i, name in enumerate(("N", "X", "Y", "A")):
        ttk.Label(frm, text=name + ":").grid(row=0, column=i * 2, padx=4, sticky="w")
        e = ttk.Entry(frm, width=8)
        e.grid(row=0, column=i * 2 + 1, padx=4, sticky="w")
        e.insert(0, defaults[name])
        ents[name] = e

    color = tk.StringVar(value="pink")  # radiobutton
    opts = ttk.LabelFrame(root, text="Цвет конфетки")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Розовый", variable=color, value="pink").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Голубой", variable=color, value="lightblue").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Жёлтый", variable=color, value="yellow").pack(
        side="left", padx=8, pady=6
    )

    canv = tk.Canvas(root, bg="white")
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def draw_candy(x, y, a, c):
        # тело — квадрат
        canv.create_rectangle(x, y, x + a, y + a, fill=c, outline="black", width=2)
        # обёртка слева
        canv.create_polygon(
            x, y, x - a // 2, y - a // 4, x - a // 2, y + a + a // 4, x, y + a,
            fill=c, outline="black", width=2,
        )
        # обёртка справа
        canv.create_polygon(
            x + a, y, x + a + a // 2, y - a // 4,
            x + a + a // 2, y + a + a // 4, x + a, y + a,
            fill=c, outline="black", width=2,
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
        cx = x
        gap = a // 4
        for _ in range(n):
            draw_candy(cx - a, y - a // 2, a, color.get())
            cx -= a + a + gap  # тело + обёртки + промежуток

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
