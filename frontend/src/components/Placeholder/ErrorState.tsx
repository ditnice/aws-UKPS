export const ErrorState = (props: React.PropsWithChildren) => {
  const { children, ...otherProps } = props
  return (
    <p role="alert" {...otherProps}>
      {children}
    </p>
  )
}
