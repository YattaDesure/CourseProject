import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №1 — Задание 2")
    root.geometry("560x340")

    ttk.Label(
        root,
        text="Задание 2.\nНа числовой оси точки A, B, C.\nКто ближе к A — B или C?",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    frm = ttk.Frame(root)
    frm.pack(fill="x", padx=10, pady=6)

    ents = {}
    for i, name in enumerate(("A", "B", "C")):
        ttk.Label(frm, text=name + ":").grid(row=i, column=0, sticky="w", padx=4, pady=2)
        e = ttk.Entry(frm, width=14)
        e.grid(row=i, column=1, sticky="w", padx=4, pady=2)
        ents[name] = e

    mode = tk.StringVar(value="info")  # radiobutton
    opts = ttk.LabelFrame(root, text="Если расстояния равны")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Сказать, что равны", variable=mode, value="info").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Брать B", variable=mode, value="b").pack(
        side="left", padx=8, pady=6
    )

    res = ttk.Label(root, text="Ответ: ")
    res.pack(anchor="w", padx=10, pady=8)

    def run():
        try:
            a = float(ents["A"].get())
            b = float(ents["B"].get())
            c = float(ents["C"].get())
        except Exception:
            messagebox.showerror("Ошибка", "Введи числа во все поля.")
            return

        db = abs(b - a)
        dc = abs(c - a)

        if db < dc:
            res.configure(text=f"Ответ: ближе B, расстояние = {db:g}")
        elif dc < db:
            res.configure(text=f"Ответ: ближе C, расстояние = {dc:g}")
        else:
            if mode.get() == "b":
                res.configure(text=f"Ответ: расстояния равны, берём B = {b}, расстояние = {db:g}")
            else:
                res.configure(text=f"Ответ: B и C на одном расстоянии = {db:g}")

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=4)
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
